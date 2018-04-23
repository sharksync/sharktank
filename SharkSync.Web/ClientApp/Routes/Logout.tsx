import * as React from 'react';
import { Route, Redirect } from 'react-router-dom';
import { RouteComponentProps } from 'react-router';
import { ApiHandlers } from '../handlers';

interface LogoutState {
    redirect: boolean;
}

export class Logout extends React.Component<RouteComponentProps<{}>, LogoutState> {

    constructor() {
        super();

        this.state = { redirect: false };

        // Clear stored auth details
        localStorage.removeItem('loggedInUserId');
        localStorage.removeItem('loggedInUserName');
        localStorage.removeItem('loggedInUserEmail');
        localStorage.removeItem('loggedInUserAvatarUrl');

        fetch(ApiHandlers.Url + 'Api/Auth/Logout', {
            credentials: 'include'
        })
            .then(response => ApiHandlers.handleErrors(response))
            .then(data => this.setState({ redirect: true }))
            .catch(error => ApiHandlers.handleCatch(error));
    }

    public render() {
        return this.state.redirect ? <Redirect to="/Console/Login" push /> : null;
    }
}
