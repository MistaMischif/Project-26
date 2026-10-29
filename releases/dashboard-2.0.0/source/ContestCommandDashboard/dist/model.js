/* Shared, pure tracking logic: calendar time never changes completion. */
(function(root){
const statuses=['todo','doing','blocked','done','deferred'];
function effective(id,records,tasks,seen=new Set()){
 const r=records[id]; if(!r)return 'todo'; if(r.status!=='done')return r.status;
 if(!r.note?.trim()||seen.has(id))return 'blocked';
 const task=tasks.find(t=>t.id===id);if(!task)return 'todo';
 const trail=new Set(seen);trail.add(id);
 return task.depends.every(d=>effective(d,records,tasks,trail)==='done')?'done':'blocked';
}
function validate(x,tasks){
 if(!x||x.schema!==1||x.plan!=='contest-command-2026'||!x.records||typeof x.records!=='object'||Array.isArray(x.records))throw Error('Not a Contest Command backup.');
 const records={}; for(const [id,r] of Object.entries(x.records)){
  if(!tasks.some(t=>t.id===id))throw Error('Backup contains an unknown task.');
  if(!r||!statuses.includes(r.status)||typeof r.note!=='string'||r.note.length>4000||typeof r.updated!=='string'||!Number.isFinite(Date.parse(r.updated)))throw Error('Invalid task record.');
  if(r.status==='done'&&!r.note.trim())throw Error('Completed task has no evidence note.');
  records[id]={status:r.status,note:r.note,updated:r.updated};
 }
 const capacity={}; for(const key of ['weekday','saturday','sunday']){
  const v=x.capacity?.[key];if(v==null||v===''){capacity[key]=null;continue;}
  if(typeof v!=='number'||!Number.isFinite(v)||v<0||v>24)throw Error('Invalid daily capacity.');capacity[key]=v;
 }
 return {schema:1,plan:'contest-command-2026',records,capacity};
}
function capacityFor(date,c){const d=new Date(date+'T12:00:00Z').getUTCDay();return c[d===0?'sunday':d===6?'saturday':'weekday']??null;}
function escape(v){return String(v??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
root.CommandModel={effective,validate,capacityFor,escape};
})(typeof module!=='undefined'?module.exports:window);
