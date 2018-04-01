import * as React from 'react';
import * as FontAwesome from 'react-fontawesome';
import { RouteComponentProps } from 'react-router';
import { DeleteButton } from '../components/DeleteButton';
import { ApiHandlers } from '../handlers';
import swal from 'sweetalert2';

interface AppsState {
    apps: App[];
    loading: boolean;
    showNewAppRow: boolean;
    newAppName: string;
    showNewAppValidationError: boolean;
}

interface GetListResponse {
    Applications: App[];
    Success: boolean;
}

interface AddResponse {
    Application: App;
    Success: boolean;
}

interface App {
    Id: string;
    Name: string;
    AccessKey: string;
}

export class Apps extends React.Component<RouteComponentProps<{}>, AppsState> {

    constructor() {
        super();
        this.state = { apps: [], loading: true, showNewAppRow: false, newAppName: '', showNewAppValidationError: false };

        fetch(ApiHandlers.Url + 'Account/Apps')
            .then(response => ApiHandlers.handleErrors(response))
            .then(response => response.json() as Promise<GetListResponse>)
            .then(data => this.setState({ apps: data.Applications, loading: false }))
            .catch(error => ApiHandlers.handleCatch(error));
    }

    public render() {
        let contents = this.state.loading ? <div className="pageLoading"><FontAwesome name="spinner" size="2x" spin /><span>Loading...</span></div> : this.renderTable(this.state.apps);

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
                        <th className="guidColumn">App Id</th>
                        <th className="guidColumn">Access Key</th>
                        <th className="actionColumn"></th>
                    </tr>
                </thead>
                <tbody>
                    {apps.map(app =>
                        <tr key={app.Id}>
                            <td>{app.Name}</td>
                            <td className="guidColumn">{app.Id}</td>
                            <td className="guidColumn">{app.AccessKey}</td>
                            <td className="actionColumn"><DeleteButton confirmMessage="Are you sure you want to delete this app?" deleteHandler={(completedCallback: () => any) => this.deleteApp(app.Id, completedCallback)} /></td>
                        </tr>
                    )}

                    {this.state.showNewAppRow ? this.renderNewAppForm() : null}
                </tbody>
            </table>

            <button className="btn btn-primary" onClick={() => this.setState({ showNewAppRow: true, newAppName: '', showNewAppValidationError: false })}>Add New App</button>
        </div >;
    }

    private renderNewAppForm() {
        return (
            <tr>
                <td>
                    <div>
                        <span className={"validation-error-tooltip " + (this.state.showNewAppValidationError ? "validation-error-tooltip-shown" : "")}>App name is required</span>
                        <input type="text" id="newAppName" placeholder="New app name" ref="appName" className={"form-control app-name-input " + (this.state.showNewAppValidationError ? "is-invalid" : "")} onChange={(event) => this.newAppNameChanged(event)} />
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

    private newAppNameChanged(event: React.ChangeEvent<HTMLInputElement>) {
        this.setState({ newAppName: event.target.value });
        if (event.target.value) {
            this.setState({ showNewAppValidationError: false });
        }
        else {
            this.setState({ showNewAppValidationError: true });
        }
    }

    private addApp() {

        if (this.state.newAppName) {
            this.setState({ showNewAppValidationError: false });

            const formData = new FormData();

            formData.append('name', this.state.newAppName);

            fetch(ApiHandlers.Url + 'Account/Apps', { method: 'POST', body: formData })
                .then(ApiHandlers.handleErrors)
                .then(response => response.json() as Promise<AddResponse>)
                .then(data => {
                    this.state.apps.push(data.Application);
                    this.setState({ showNewAppRow: false });
                }).catch(error => {
                    this.setState({ showNewAppRow: false });
                });
        }
        else {
            this.setState({ showNewAppValidationError: true });
        }
    }

    private deleteApp(appId: string, completedCallback: () => any) {

        const formData = new FormData();

        formData.append('id', appId);

        fetch(ApiHandlers.Url + 'Account/Apps', { method: 'DELETE', body: formData })
            .then(ApiHandlers.handleErrors)
            .then(response => {

                var index = -1;
                for (var i = 0; i < this.state.apps.length; i++) {
                    if (this.state.apps[i].Id === appId) {
                        index = i;
                        break;
                    }
                }
                this.state.apps.splice(index, 1);
                this.setState({ apps: this.state.apps });
            }).catch(error => {
                completedCallback();
            });
    }
}
