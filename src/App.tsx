import { useEffect, useState } from "react";

function App() {
  const [socket, setSocket] = useState<WebSocket | undefined>(undefined);
  const [messages, setMessages] = useState<string[]>([]);

  useEffect(() => {
    const newSocket = new WebSocket("ws://localhost:5238/ws");
    setSocket(newSocket);

    // Connection opened
    newSocket.addEventListener("open", (event) => {
      console.log("Connected to server");
      newSocket.send("Hello Server!");
    });

    // Listen for messages
    newSocket.addEventListener("message", (event) => {
      setMessages((oldMessages) => [...oldMessages, event.data]);
      console.log("Message from server ", event.data);
    });
  }, []);

  return (
    <>
      <h1>Websocket Chat</h1>
      <button onClick={() => socket?.send("Clicked a button")}>
        Send Message
      </button>
      <div>
        {messages.map((m, i) => {
          return <div key={i}>{m}</div>;
        })}
      </div>
    </>
  );
}

export default App;
