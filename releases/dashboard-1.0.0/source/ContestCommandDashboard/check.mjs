import fs from 'node:fs';
import assert from 'node:assert/strict';
import vm from 'node:vm';
const data=JSON.parse(fs.readFileSync('dist/data.json','utf8'));
const sandbox={module:{exports:{}}};vm.runInNewContext(fs.readFileSync('dist/model.js','utf8'),sandbox);
const M=sandbox.module.exports.CommandModel,ids=new Set(data.tasks.map(t=>t.id));
assert.equal(ids.size,data.tasks.length);
for(const t of data.tasks){assert(t.minutes>0);for(const dep of t.depends){assert(ids.has(dep),`Missing ${dep}`);assert(data.tasks.find(x=>x.id===dep).date<=t.date,`Future prerequisite ${dep}`);}}
function visit(id,path=[]){assert(!path.includes(id),'Dependency cycle');for(const dep of data.tasks.find(t=>t.id===id).depends)visit(dep,[...path,id]);}
data.tasks.forEach(t=>visit(t.id));
const sample=[{id:'a',depends:[]},{id:'b',depends:['a']},{id:'c',depends:['b']}],r={};
assert.equal(M.effective('a',r,sample),'todo');
for(const id of ['a','b','c'])r[id]={status:'done',note:'Test evidence',updated:new Date().toISOString()};
assert.equal(M.effective('c',r,sample),'done');r.a.status='blocked';assert.equal(M.effective('c',r,sample),'blocked');
r.a.status='done';r.a.note='';assert.equal(M.effective('c',r,sample),'blocked');
assert.equal(M.capacityFor('2026-10-25',{weekday:8,saturday:6,sunday:0}),0);
assert.equal(M.capacityFor('2026-10-26',{weekday:8,saturday:6,sunday:4}),8);
assert.throws(()=>M.validate({schema:1,plan:'contest-command-2026',records:r},sample));
assert.throws(()=>M.validate({schema:1,plan:'contest-command-2026',records:{unknown:{status:'todo',note:'',updated:new Date().toISOString()}}},sample));
assert.equal(M.escape('<script>'), '&lt;script&gt;');
const totals={};for(const t of data.tasks.filter(t=>t.date<='2026-11-18'))totals[t.date]=(totals[t.date]||0)+t.minutes;
assert.equal(Object.values(totals).reduce((s,x)=>s+x,0)/60,178.5);
assert.equal(new Date(data.projects[2].deadline).toISOString(),'2026-11-16T01:59:00.000Z');
assert.equal(new Date(data.projects[3].deadline).toISOString(),'2026-11-18T20:00:00.000Z');
console.log(JSON.stringify({checks:'passed: data integrity, dependencies, regressions, zero capacity, import rejection, escaping, deadlines',tasks:data.tasks.length,peakHours:Math.max(...Object.values(totals))/60,over8:Object.entries(totals).filter(([d,m])=>m>480).map(([d,m])=>[d,m/60])}));
