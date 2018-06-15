import * as React from 'react';
import { RouteComponentProps, Redirect } from 'react-router';
import { ApiHandlers } from '../handlers';
import { Auth, LoggedInUser } from '../auth';

interface LoginCompleteState {
    prefetchingUserDetails: boolean
}

export class LoginComplete extends React.Component<RouteComponentProps<{}>, LoginCompleteState> {

    constructor() {
        super();
        
        this.state = { prefetchingUserDetails: true };
    }

    public componentWillMount() {

        Auth.putLoggedInUserIntoCache((loggedInUser) => {
            
            this.setState({ prefetchingUserDetails: false });

        });
    }

    public render() {

        if (!this.state.prefetchingUserDetails)
            return <Redirect to='/Console/Apps' />;
        else
            return null;
    }
}
