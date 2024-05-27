const WebSocket = require('ws');

const PORT = 3003; 

class StoreServer {
    constructor() {
        try {
            this.wss = new WebSocket.Server({ port: PORT });
            this.userList = new Map();

            this.wss.on('connection', this.handleConnection.bind(this));
        } catch (error) {
            console.error("Error starting StoreServer:", error);
        }
    }

    handleConnection(socket) {
        this.userList.set(socket, "");

        socket.on('message', (message) => {
            this.handleMessage(socket, message.toString());
        });

        socket.on('close', () => {
            this.handleUserDisconnect(socket);
        });
    }

    handleMessage(socket, message) {
        const json = JSON.parse(message);

        if (json.type === 'store') {
            this.sendStoreStatus(socket);
        } 
        else {
            console.error("INCORRECT JSON TYPE");
        }
    }

    sendStoreStatus(socket) {
        const statusMessage = { type: 'store', status: true };
        socket.send(JSON.stringify(statusMessage));
        console.log("Sending store status: " + JSON.stringify(statusMessage));
    }

    handleUserDisconnect(socket) {
        this.userList.delete(socket);
        console.log("User disconnected from Store Server");
    }
}

const gameServer = new StoreServer();

console.log("Store server iniciado en el puerto " + PORT);
