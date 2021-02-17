import { useState } from 'react';

import './App.css';

import { GreeterPromiseClient } from './grpc/greet_grpc_web_pb';
import { HelloRequest } from './grpc/greet_pb';

const authInterceptor = {
  intercept: (request, invoker) => {
    const metadata = request.getMetadata()
    metadata.Authorization = 'Bearer --xyz--'
    return invoker(request)
  }
}

const options = {
  unaryInterceptors: [authInterceptor],
  streamInterceptors: [authInterceptor]
}

const grpcClientAync = new GreeterPromiseClient('https://localhost:5001', null, options);

function App() {

  let [{ requestText, replyText }, setState] = useState({ requestText: "John Doe", replyText: null });

  const callGrpcService = async () => {
    const request = new HelloRequest();
    request.setName(requestText);

    try {
      const response = await grpcClientAync.sayHello(request);
      setState({ requestText, replyText: response.getMessage() });
    }
    catch(err) {
      setState({ requestText, replyText: `Error: ${err.code} - ${err.message}` });
    }
  }

  const updateRequestText = (value) => {
    setState({ requestText: value, replyText });
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
