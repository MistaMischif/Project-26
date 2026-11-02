(function(root){
'use strict';
const statuses=['todo','doing','blocked','pass','fail'];
function latest(events,id,hash,task){return events.filter(e=>e.id===id&&(task.scope==='submission'||e.hash===hash)).at(-1);}
function status(tasks,events,id,hash,seen=[]){
 const task=tasks.find(t=>t.id===id);if(!task||seen.includes(id))return 'blocked';
 const record=latest(events,id,hash,task);if(!record)return 'todo';if(record.status!=='pass')return record.status;
 return task.depends.every(d=>status(tasks,events,d,hash,[...seen,id])==='pass')?'pass':'blocked';
}
function validate(value,tasks){
 if(!value||value.schema!==1||value.plan!=='living-table-chess-2026'||!Array.isArray(value.events)||value.events.length>10000)throw Error('Not a valid chess rundown backup.');
 if(!/^[a-f0-9]{64}$/i.test(value.hash))throw Error('Candidate must have a full SHA-256.');
 const ids=new Set();
 const events=value.events.map(e=>{
  if(!e||!tasks.some(t=>t.id===e.id)||typeof e.uid!=='string'||ids.has(e.uid)||!statuses.includes(e.status)||typeof e.note!=='string'||!e.note.trim()||e.note.length>4000||typeof e.at!=='string'||!Number.isFinite(Date.parse(e.at))||typeof e.tester!=='string'||!e.tester.trim()||e.tester.length>100||!/^[a-f0-9]{64}$/i.test(e.hash))throw Error('Invalid or duplicate observation.');
  ids.add(e.uid);return {id:e.id,uid:e.uid,status:e.status,note:e.note,at:e.at,tester:e.tester,hash:e.hash.toLowerCase()};
 });
 return {schema:1,plan:'living-table-chess-2026',hash:value.hash.toLowerCase(),events};
}
function escape(s){return String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
root.ContestRundown={status,latest,validate,escape};
})(typeof module==='undefined'?window:module.exports);
