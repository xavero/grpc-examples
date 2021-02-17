import { HelloRequest } from './grpc/greet_pb.js';
import { GreeterPromiseClient } from './grpc/greet_grpc_web_pb.js';

const greetService = new GreeterPromiseClient('https://localhost:5001');

document.getElementById('sendCommand').addEventListener('click', async () => {

    let nameText = document.getElementById('requestText').value || 'John Doe';

    const request = new HelloRequest();

    request.setName(nameText);

    const responseText = document.getElementById('responseText');

    try {
        const response = await greetService.sayHello(request);
        responseText.innerText = `Response: ${response.getMessage()}`;
    } catch (err) {
        responseText.innerText = `Error ${err.code} - ${err.message}`;
    }
});
