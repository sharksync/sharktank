import * as React from 'react';
import { ApiHandlers } from '../handlers';
import { Link, NavLink } from 'react-router-dom';

interface NavMenuState {
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

export class NavMenu extends React.Component<{}, NavMenuState> {

    public updateLoggedInState() {

        var id = localStorage.getItem('loggedInUserId');
        var name = localStorage.getItem('loggedInUserName');
        var email = localStorage.getItem('loggedInUserEmail');
        var avatarUrl = localStorage.getItem('loggedInUserAvatarUrl');

        var currentUrl = window.location.href.toLowerCase();
        var onSignInOrSignOutPage = currentUrl.indexOf("/console/login") > -1 || currentUrl.indexOf("/console/logout") > -1; 

        if (id && name && email && avatarUrl && !onSignInOrSignOutPage) {

            this.state = {
                loggedInUser: {
                    Id: id,
                    Name: name,
                    EmailAddress: email,
                    AvatarUrl: avatarUrl
                }
            };
        }
        else {

            if (!onSignInOrSignOutPage) {

                fetch(ApiHandlers.Url + 'Api/Auth/Details', {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Cache': 'no-cache'
                    },
                    credentials: 'include'
                })
                    .then(response => response.json() as Promise<AuthDetailsResponse>)
                    .then(data => {
                        this.setState({ loggedInUser: data.LoggedInUser });

                        localStorage.setItem('loggedInUserId', data.LoggedInUser.Id);
                        localStorage.setItem('loggedInUserName', data.LoggedInUser.Name);
                        localStorage.setItem('loggedInUserEmail', data.LoggedInUser.EmailAddress);
                        localStorage.setItem('loggedInUserAvatarUrl', data.LoggedInUser.AvatarUrl);
                    });
            }

            this.state = { loggedInUser: null };
        }

    }

    public render() {
        this.updateLoggedInState();

        return <div className='main-nav'>
            <div className='navbar navbar-inverse'>
                <div className='navbar-header'>
                    <button type='button' className='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>
                        <span className='sr-only'>Toggle navigation</span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                    </button>
                    <Link className='navbar-brand' to={'/'}>SharkSync.io</Link>
                </div>
                <div className='clearfix'></div>
                <div className='navbar-collapse collapse'>
                    {this.state.loggedInUser ? this.renderLoggedInMenu() : this.renderLoggedOutMenu() }
                </div>
                <div className='clearfix'></div>
                <div className='navbar-profile navbar-collapse collapse'>
                    {this.state.loggedInUser ? this.renderProfile(this.state.loggedInUser) : null}
                </div>
            </div>
        </div>;
    }

    public renderLoggedInMenu() {
        return <ul className='nav navbar-nav'>
            <li>
                <NavLink to={'/Console/Apps'} exact activeClassName='active'>
                    <span className='glyphicon glyphicon-home'></span> Apps
                </NavLink>
            </li>
        </ul>;
    }

    public renderLoggedOutMenu() {
        return <ul className='nav navbar-nav'>
            <li>
                <NavLink to={'/Console/Login'} exact activeClassName='active'>
                    <span className='glyphicon glyphicon-log-in'></span> Login
                </NavLink>
            </li>
        </ul>;
    }

    public renderProfile(loggedInUser: LoggedInUser) {
        return <ul className='nav navbar-nav'>
            <li>
                <img src={loggedInUser.AvatarUrl} height="32px" width="32px" className="avatar" />              
                <span>{loggedInUser.Name}</span>
                <NavLink to={'/Console/Logout'} exact activeClassName='active' className="logout">
                    <span className='glyphicon glyphicon-log-out'></span> Logout
                </NavLink>
            </li>
        </ul>;
    }
}
