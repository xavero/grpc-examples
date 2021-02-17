import { HelloRequest } from './grpc/greet_pb.js';
import { GreeterClient } from './grpc/greet_grpc_web_pb.js';

const echoService = new GreeterClient('https://localhost:5001');

document.getElementById('sendCommand').addEventListener('click', () => {

    let nameText = document.getElementById('requestText').value || 'John Doe';

    const request = new HelloRequest();

    request.setName(nameText);
    
    echoService.sayHello(request, {}, (err, response) => {
        const responseText = document.getElementById('responseText');
        if (err) {
            responseText.innerText = `Error ${err.code} - ${err.message}`;
        } else {
            responseText.innerText = `Response: ${response.getMessage()}`;
        }
    });
});
