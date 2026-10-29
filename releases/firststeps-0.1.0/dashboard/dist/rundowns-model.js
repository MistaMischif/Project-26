(function(root){'use strict';
const statuses=['todo','doing','done','blocked','deferred'];
function escape(s){return String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}
function validate(v,tasks){if(!v||v.schema!==2||v.plan!=='contest-command-rundowns'||!v.records||typeof v.records!=='object'||Array.isArray(v.records))throw Error('Not a rundown backup.');const records={};for(const [id,r]of Object.entries(v.records)){if(!tasks.some(t=>t.id===id)||!r||!statuses.includes(r.status)||typeof r.note!=='string'||r.note.length>4000||!Number.isFinite(Date.parse(r.updated)))throw Error('Invalid task record.');records[id]={status:r.status,note:r.note,updated:r.updated};}let capacity=v.capacity??null;if(capacity!==null&&(typeof capacity!=='number'||!Number.isFinite(capacity)||capacity<0||capacity>24))throw Error('Invalid capacity.');return{schema:2,plan:'contest-command-rundowns',records,capacity};}
function merge(a,b){const out={...a};for(const[id,r]of Object.entries(b))if(!out[id]||Date.parse(r.updated)>Date.parse(out[id].updated))out[id]=r;return out;}
function dayKey(now=new Date()){return new Intl.DateTimeFormat('en-CA',{timeZone:'America/Chicago',year:'numeric',month:'2-digit',day:'2-digit'}).format(now);}
root.RundownModel={escape,validate,merge,dayKey,statuses};
})(typeof module==='undefined'?window:module.exports);
