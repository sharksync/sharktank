import * as React from 'react';
import { NavMenu } from './NavMenu';
import { ApiHandlers } from '../handlers';
import { RouteProps } from 'react-router-dom';

interface LayoutProps {
    children?: React.ReactNode;
}

interface LayoutState {
    loggedInUser: LoggedInUser | null;
}

interface AuthDetailsResponse {
    LoggedInUser: LoggedInUser;
    Success: boolean;
}

export interface LoggedInUser {
    Id: string;
    Name: string;
    EmailAddress: string;
    AvatarUrl: string;
}

export class Layout extends React.Component<{}, LayoutState> {

    constructor() {
        super();

        

        var id = localStorage.getItem('loggedInUserId');
        var name = localStorage.getItem('loggedInUserName');
        var email = localStorage.getItem('loggedInUserEmail');
        var avatarUrl = localStorage.getItem('loggedInUserAvatarUrl');

        if (id && name && email && avatarUrl) {

            this.state = {
                loggedInUser: {
                    Id: id,
                    Name: name,
                    EmailAddress: email,
                    AvatarUrl: avatarUrl
                }
            };
        }
        //else if (this.props.location == null || this.props.location.key != 'Login') {

           this.state = { loggedInUser: null };

            //fetch(ApiHandlers.Url + 'Api/Auth/Details', {
            //    credentials: 'include'
            //})
            //    .then(response => ApiHandlers.handleErrors(response))
            //    .then(response => response.json() as Promise<AuthDetailsResponse>)
            //    .then(data => {
            //        this.setState({ loggedInUser: data.LoggedInUser });

            //        localStorage.setItem('loggedInUserId', data.LoggedInUser.Id);
            //        localStorage.setItem('loggedInUserName', data.LoggedInUser.Name);
            //        localStorage.setItem('loggedInUserEmail', data.LoggedInUser.EmailAddress);
            //        localStorage.setItem('loggedInUserAvatarUrl', data.LoggedInUser.AvatarUrl);
            //    })
            //    .catch(error => ApiHandlers.handleCatch(error));

        //}
    }

    public render() {
        return <div className='container-fluid'>
            <div className='row'>
                <div className='col-sm-3'>
                    <NavMenu loggedInUser={this.state.loggedInUser} />
                </div>
                <div className='col-sm-9'>
                    { this.props.children }
                </div>
            </div>
        </div>;
    }
}
