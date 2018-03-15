declare namespace signalR {
    export class HubConnection {
        on<T>(name: string, callback: (T) => void): void;
    }
}
declare namespace Slidable.Hub {

    export var hubConnection: signalR.HubConnection | null;

    export function onConnected(callback: Function): void;

    export function onDisconnected(callback: Function): void;

    export function connect(): void;
}