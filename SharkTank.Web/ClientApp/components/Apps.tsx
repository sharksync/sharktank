import * as React from 'react';
import { RouteComponentProps } from 'react-router';

interface AppsState {
    apps: App[];
    loading: boolean;
}

export class Apps extends React.Component<RouteComponentProps<{}>, AppsState> {
    constructor() {
        super();
        this.state = { apps: [], loading: true };

        fetch('api/apps')
            .then(response => response.json() as Promise<App[]>)
            .then(data => {
                this.setState({ apps: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading ? <p><em>Loading...</em></p> : Apps.renderTable(this.state.apps);

        return <div>
            <h1>Your Apps</h1>

            {contents}

        </div>;
    }

    private static renderTable(apps: App[]) {
        return <div className="table-responsive">
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th>App Id</th>
                        <th>Access Key</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {apps.map(app =>
                        <tr>
                            <td>{app.appId}</td>
                            <td>{app.accessKey}</td>
                            <td><button className="btn btn-danger" onClick={() => { Apps.deleteApp(app.appId) }}>Delete</button></td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>;
    }

    static deleteApp(appId: string) {

    }
}

interface App {
    appId: string;
    accessKey: string;
}
