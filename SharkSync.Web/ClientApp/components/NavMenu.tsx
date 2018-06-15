import * as React from 'react';
import { Link, NavLink } from 'react-router-dom';
import { ApiHandlers } from '../handlers';
import { Auth, LoggedInUser } from '../auth';

export class NavMenu extends React.Component<{}, {}> {

    constructor() {
        super();

    }

    public render() {

        var loggedInUser = Auth.getLoggedInUserFromCache();

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
                    {loggedInUser ? this.renderLoggedInMenu() : this.renderLoggedOutMenu() }
                </div>
                <div className='clearfix'></div>
                <div className='navbar-profile navbar-collapse collapse'>
                    {loggedInUser ? this.renderProfile(loggedInUser) : null}
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
