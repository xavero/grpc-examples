import { useState } from 'react';

import './App.css';

import { GreeterClient } from './grpc/greet_grpc_web_pb';
import { HelloRequest, HelloReply } from './grpc/greet_pb';

const grpcClient = new GreeterClient('https://localhost:5001', null, null);

function App() {

  const callGrpcService = () => {
    const request = new HelloRequest();
    request.setName(requestText);

    grpcClient.sayHello(request, {}, (err, response) => {
        replyText = err || response.getMessage();

        setState({requestText, replyText});
    });
  }

  let [{requestText, replyText}, setState] = useState({ requestText: "John Doe", replyText: null  });
  
  const updateRequestText = (value) => {
    setState({requestText: value, replyText});
  };

  return (
    <div className="App">
      <header className="App-header">
        <p>
          <input
            type="text"
            value={requestText}
            onChange={e => updateRequestText(e.target.value)}
          />
        </p>
        <p>
          <button style={{ padding: 10 }}
            onClick={callGrpcService}>Click for grpc request</button>
        </p>
        {replyText && <p>{replyText}</p>}
      </header>
    </div>
  );
}

export default App;
