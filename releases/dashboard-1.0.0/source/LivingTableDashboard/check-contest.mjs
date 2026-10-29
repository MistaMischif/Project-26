import fs from 'node:fs';import vm from 'node:vm';import assert from 'node:assert/strict';
const box={window:{},module:{exports:{}}};vm.runInNewContext(fs.readFileSync('dist/contest-plan.js','utf8'),box);vm.runInNewContext(fs.readFileSync('dist/contest-model.js','utf8'),box);
const D=box.window.CONTEST_PLAN,M=box.module.exports.ContestRundown,ids=new Set(D.tasks.map(t=>t.id));
assert.equal(ids.size,29);assert.equal(D.tasks.length,29);
function visit(id,seen=[]){assert(!seen.includes(id),'Cycle');const t=D.tasks.find(t=>t.id===id);assert(t);assert(t.steps.length>=3&&t.passCriteria&&t.evidence);for(const dep of t.depends){assert(ids.has(dep));assert(D.tasks.find(t=>t.id===dep).date<=t.date);visit(dep,[...seen,id]);}}
D.tasks.forEach(t=>visit(t.id));
const hash='a'.repeat(64),other='b'.repeat(64),tasks=[{id:'a',scope:'build',depends:[]},{id:'b',scope:'build',depends:['a']}];
const event=(id,status='pass',h=hash)=>({id,uid:id+status+h,status,hash:h,note:'Captured run',tester:'QA',at:'2026-10-29T12:00:00Z'});
let events=[event('a'),event('b')];assert.equal(M.status(tasks,events,'b',hash),'pass');assert.equal(M.status(tasks,events,'b',other),'todo');events.push(event('a','fail'));assert.equal(M.status(tasks,events,'b',hash),'blocked');
const valid={schema:1,plan:'living-table-chess-2026',hash,events};assert.equal(M.validate(valid,tasks).events.length,3);
assert.throws(()=>M.validate({...valid,events:[{...event('a'),note:''}]},tasks));assert.throws(()=>M.validate({...valid,events:[event('a'),event('a')]},tasks));assert.throws(()=>M.validate({...valid,hash:'123'},tasks));assert.throws(()=>M.validate({...valid,events:[event('unknown')]},tasks));assert.equal(M.escape('<script>'),'&lt;script&gt;');
assert.equal(new Date(D.deadline).toISOString(),'2026-11-18T20:00:00.000Z');assert.equal(new Date(D.target).toISOString(),'2026-11-17T23:00:00.000Z');
console.log('PASS: 29 detailed checks, dependency DAG/dates, build invalidation, prerequisite reopening, import validation, escaping and Central-time deadlines.');
