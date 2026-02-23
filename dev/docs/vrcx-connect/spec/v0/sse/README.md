# Server-sent events (SSE) in VRCX Connect Protocol

VRCX Connect Protocol use server-sent events (SSE) for send events to client.

In short, SSE is a special long-running http response with header `Content-Type: text/event-stream`. The response body will be:

```plaintext
event: server
data: 4d896d95b55478
id: 1

event: time
data: 2026-02-23T04:43:48Z
id: 2
```

NOTE: SSE is a server-to-client one way connection.

You can use the [`EventSource`](https://developer.mozilla.org/en-US/docs/Web/API/EventSource) Javascript Web API to listen event from SSE endpoint.

For more details about SSE, see following documents:

- [MDN: Using server-sent Events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events)
- [WebSocket.org: WebSockets vs Server-Sent Events (SSE): Choosing Your Real-Time Protocol](https://websocket.org/comparisons/sse/)

## Endpoint

```plaintext
http://127.0.0.1:34583/connect-v0/event-source
```

## Events

All events will be serialize into JSON Object. For example.

```plaintext
event: notification
data: {"title":"Notification Title", "message":"Notification Message"}
id: 114514
```

For full events type, see [Events](events.md).
