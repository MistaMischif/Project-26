import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createHost,validateCard,extractText} from './server.mjs';

test('card recognition rejects uncertainty and invalid labels',()=>{
  assert.deepEqual(validateCard({color:'Blue',count:2,confidence:.95}),{color:3,count:2,picture:'',confidence:.95});
  assert.equal(validateCard({picture:'Plumpy',confidence:.9}).color,6);
  for(const card of [{color:'Magenta',count:1,confidence:1},{color:'Red',count:3,confidence:1},{color:'Blue',count:1,confidence:NaN}])assert.throws(()=>validateCard(card));
});
test('extract responses text',()=>{
  assert.equal(extractText({output:[{content:[{type:'output_text',text:'Hello!'}]}]}),'Hello!');
});
test('relay protects provider credentials and forwards documented formats',async()=>{
  const calls=[];
  const server=createHost({env:{MODEL_API_KEY:'provider-secret',WIT_SERVER_TOKEN:'wit-secret',LIVING_TABLE_DEVICE_TOKEN:'device-test'},fetchImpl:async(url,options)=>{
    calls.push({url,options});
    if(url.includes('/asr/'))return Response.json({transcript:'Hello Plumpy'});
    if(url.includes('/synthesize'))return new Response(new Uint8Array([82,73,70,70]),{headers:{'Content-Type':'audio/wav'}});
    const request=JSON.parse(options.body);
    return Response.json({output_text:request.instructions.startsWith('Identify')?'bad recognition':'Welcome to the plum trees!'});
  }});
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const base=`http://127.0.0.1:${server.address().port}`;
  const post=(route,input,token='device-test')=>fetch(base+route,{method:'POST',headers:{Authorization:`Bearer ${token}`,'Content-Type':'application/json'},body:JSON.stringify(input)});
  try {
    let response=await post('/chat',{text:'Hi',character:'Plumpy'},'wrong');assert.equal(response.status,401);assert.equal(calls.length,0);
    response=await post('/chat',{text:'Hi',character:'Plumpy',state:'Turn 0',history:[]});assert.equal(response.status,200);
    assert.equal((await response.json()).text,'Welcome to the plum trees!');
    let sent=JSON.parse(calls[0].options.body);assert.equal(sent.model,'muse-spark-1.3');assert.equal(sent.store,false);assert.equal(sent.input[0].content,'Hi');
    assert.equal(calls[0].options.headers.Authorization,'Bearer provider-secret');
    response=await post('/recognize',{image:'YWJj'});assert.equal((await response.json()).confidence,0);
    response=await post('/chat',{text:'Hi',character:'Unknown'});assert.equal(response.status,400);
    response=await post('/speak',{text:'Welcome!'});assert.equal(response.status,200);await response.arrayBuffer();
    const speech=calls.find(c=>c.url.includes('/synthesize'));assert.equal(JSON.parse(speech.options.body).q,'Welcome!');assert.equal(speech.options.headers.Accept,'audio/wav');
    const wav=Buffer.alloc(44);wav.write('RIFF');wav.write('WAVE',8);
    response=await fetch(base+'/transcribe',{method:'POST',headers:{Authorization:'Bearer device-test'},body:wav});assert.equal((await response.json()).text,'Hello Plumpy');
    const transcription=calls.find(c=>c.url.includes('/asr/'));assert.equal(JSON.parse(await transcription.options.body.get('request').text()).audioEncoding,'WAV');
  } finally {server.closeAllConnections();await new Promise(resolve=>server.close(resolve));}
});
test('missing provider setup is explicit, never a pretend AI response',async()=>{
  const server=createHost({env:{LIVING_TABLE_DEVICE_TOKEN:'test'},fetchImpl:()=>{throw new Error('Must not call provider');}});
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  try {
    const response=await fetch(`http://127.0.0.1:${server.address().port}/chat`,{method:'POST',headers:{Authorization:'Bearer test'},body:JSON.stringify({text:'Hi',character:'Jolly'})});
    assert.equal(response.status,503);assert.match((await response.json()).error,/not configured/);
  } finally {server.closeAllConnections();await new Promise(resolve=>server.close(resolve));}
});
