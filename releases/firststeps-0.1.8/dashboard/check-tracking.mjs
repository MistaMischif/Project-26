import vm from 'node:vm';
import fs from 'node:fs';
import assert from 'node:assert/strict';
const nodes=new Map();
function element(id){if(!nodes.has(id))nodes.set(id,{id,value:id==='filter'?'all':'',hidden:false,innerHTML:'',textContent:'',classList:{toggle(){}},setAttribute(){},removeAttribute(){},addEventListener(){}});return nodes.get(id)}
const context=vm.createContext({window:{addEventListener(){},scrollTo(){}},document:{getElementById:element,querySelector:element,querySelectorAll:()=>[]},localStorage:{getItem:()=>null,setItem(){}},location:{hash:'#overview'},console,Date,crypto:{randomUUID:()=>String(Math.random())},setTimeout,URL,Blob});
vm.runInContext(fs.readFileSync('dist/evidence.js','utf8'),context);
vm.runInContext(fs.readFileSync('dist/app.js','utf8'),context);
function run(code){return vm.runInContext(code,context)}
assert.equal(run('summary().pass'),0);
run("events=tests.map((t,i)=>({id:String(i),test:t.id,status:'pass',note:'QA',device:'Quest 3',hash:E.build.sha256,at:'2026-10-25T22:00:00Z'}));render();");
assert.equal(run('summary().pass'),8);
assert.equal(element('gate-title').textContent,'Headset checklist passed');
run("events.push({...events[0],id:'retest',status:'fail',at:'2026-10-25T22:01:00Z'});render();");
assert.equal(run('summary().pass'),7);assert.equal(run('summary().fail'),1);
assert.equal(element('gate-title').textContent,'Headset issues need attention');
run("events.push({...events[1],id:'other-apk',hash:'a'.repeat(64),status:'fail',at:'2026-10-25T22:02:00Z'});render();");
assert.equal(run('summary().fail'),1);
assert.equal(run("validEvent({...events[0],status:'invented'})"),false);
assert.equal(run("validEvent({...events[0],note:''})"),false);
assert.equal(run("validEvent({...events[0],at:'bad date'})"),false);
assert.equal(run("esc('<script>')"),'&lt;script&gt;');
run("events.push({...events[0],id:'invalidate',status:'not-run',at:'2026-10-25T22:03:00Z'});render();");
assert.equal(run('summary().fail'),0);assert.equal(run("summary()['not-run']"),1);
console.log('PASS: progress, retest regression, build isolation, invalidation, input validation, and text escaping.');
