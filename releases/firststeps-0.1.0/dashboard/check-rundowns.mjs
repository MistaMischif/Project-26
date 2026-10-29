import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
const c={window:{}};vm.runInNewContext(fs.readFileSync('dist/rundowns-data.js','utf8'),c);const d=c.window.RUNDOWNS;
const {RundownModel:m}=createRequire(import.meta.url)('./dist/rundowns-model.js');
const ids=new Set(d.tasks.map(t=>t.id));assert.equal(ids.size,d.tasks.length);assert.equal(d.projects.length,6);
for(const t of d.tasks){assert(t.steps.length>=3);assert(t.done);assert(d.projects.some(p=>p.id===t.project));for(const dep of t.depends)assert(ids.has(dep),dep);}
const visit=(id,path=[])=>{assert(!path.includes(id),'dependency cycle');for(const dep of d.tasks.find(t=>t.id===id).depends)visit(dep,[...path,id]);};for(const id of ids)visit(id);
const first=d.tasks[0].id,rec={status:'done',note:'',updated:'2026-10-29T18:00:00Z'},base={schema:2,plan:'contest-command-rundowns',records:{[first]:rec},capacity:null};assert.equal(m.validate(base,d.tasks).records[first].status,'done');assert.throws(()=>m.validate({...base,capacity:25},d.tasks));assert.throws(()=>m.validate({...base,records:{unknown:rec}},d.tasks));assert.equal(m.merge({[first]:rec},{[first]:{...rec,status:'todo',updated:'2026-10-28T18:00:00Z'}})[first].status,'done');
assert.equal(m.dayKey(new Date('2026-11-05T01:59:00Z')),'2026-11-04');
const book=['# Contest Command — full execution plan','Proposed schedule, October 29, 2026. Progress notes are optional.'];for(const p of d.projects){book.push('\n## '+p.name,p.pitch,'\nTrack: '+p.track,'\nBuild: '+p.build,'\nDemo: '+p.demo,'\nCut: '+p.cut);for(const t of d.tasks.filter(t=>t.project===p.id))book.push('\n### '+t.date+' — '+t.id+' '+t.title,...t.steps.map((s,i)=>(i+1)+'. '+s),'Done when: '+t.done);}
fs.writeFileSync('RUNBOOK.md',book.join('\n\n'));console.log(JSON.stringify({passed:true,projects:d.projects.length,tasks:d.tasks.length}));

if(d.publishedRecords){assert.equal(Object.keys(m.validate({schema:2,plan:'contest-command-rundowns',records:d.publishedRecords,capacity:null},d.tasks).records).length,Object.keys(d.publishedRecords).length);}
