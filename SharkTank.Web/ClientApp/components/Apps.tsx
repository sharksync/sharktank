import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import { DeleteButton } from './DeleteButton';
import { ApiHandlers } from '../handlers';
import swal from 'sweetalert2';

interface AppsState {
    apps: App[];
    loading: boolean;
    showNewAppRow: boolean;
}

export class Apps extends React.Component<RouteComponentProps<{}>, AppsState> {

    constructor() {
        super();
        this.state = { apps: [], loading: true, showNewAppRow: false };

        fetch('api/apps')
            .then(ApiHandlers.handleErrors)
            .then(response => response.json() as Promise<App[]>)
            .then(data => {
                this.setState({ apps: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading ? <p><em>Loading...</em></p> : this.renderTable(this.state.apps);

        return <div>
            <h1>Your Apps</h1>

            {contents}

        </div>;
    }

    private renderTable(apps: App[]) {
        return <div className="table-responsive">
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>App Id</th>
                        <th>Access Key</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {apps.map(app =>
                        <tr key={app.appId}>
                            <td>My App</td>
                            <td>{app.appId}</td>
                            <td>{app.accessKey}</td>
                            <td><DeleteButton deleteHandler={(completedCallback: () => any) => this.deleteApp(app.appId, completedCallback)} /></td>
                        </tr>
                    )}

                    {this.state.showNewAppRow ? this.renderNewAppForm() : null}
                </tbody>
            </table>

            <button className="btn btn-primary" onClick={() => this.setState({ showNewAppRow: true })}>Add New App</button>
        </div >;
    }

    private renderNewAppForm() {
        return (
            <tr>
                <td>
                    <div className="form-group">
                        <span className="validation-error-tooltip">Tooltip text</span>
                        <input type="text" id="newAppName" placeholder="New app name" ref="appName" className="form-control is-invalid" />
                    </div>
                </td>
                <td colSpan={3}>
                    <div className="btn-toolbar">
                        <button type="button" className="btn btn-success" onClick={() => this.addApp()}>Save</button>
                        <button type="button" className="btn btn-info" onClick={() => this.setState({ showNewAppRow: false })}>Cancel</button>
                    </div>
                </td>
            </tr>
        )
    }

    private addApp() {



    }

    private deleteApp(appId: string, completedCallback: () => any) {

        const formData = new FormData();

        formData.append('id', appId);

        fetch('api/apps', { method: 'DELETE', body: formData })
            .then(ApiHandlers.handleErrors)
            .then(response => {

                var index = -1;
                for (var i = 0; i < this.state.apps.length; i++) {
                    if (this.state.apps[i].appId === appId) {
                        index = i;
                        break;
                    }
                }
                this.state.apps.splice(index, 1);
                this.setState({ apps: this.state.apps });

                completedCallback();
            }).catch(error => {
                completedCallback();
            });
    }
}

interface App {
    appId: string;
    accessKey: string;
}
