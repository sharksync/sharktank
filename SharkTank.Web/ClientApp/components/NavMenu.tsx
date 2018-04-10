import * as React from 'react';
import { Link, NavLink } from 'react-router-dom';
import * as Layout from 'ClientApp/components/Layout';

interface NavMenuState {
    loggedInUser: Layout.LoggedInUser | null;
}

export class NavMenu extends React.Component<NavMenuState, {}> {
    public render() {
        return <div className='main-nav'>
            <div className='navbar navbar-inverse'>
                <div className='navbar-header'>
                    <button type='button' className='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>
                        <span className='sr-only'>Toggle navigation</span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                        <span className='icon-bar'></span>
                    </button>
                    <Link className='navbar-brand' to={'/'}>Shark Sync</Link>
                </div>
                <div className='clearfix'></div>
                <div className='navbar-collapse collapse'>
                    {this.props.loggedInUser ? this.renderLoggedInMenu() : this.renderLoggedOutMenu() }
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
            <li>
                <NavLink to={'/Console/Logout'} exact activeClassName='active'>
                    <span className='glyphicon glyphicon-log-out'></span> Logout
                </NavLink>
            </li>
        </ul>;
    }

    public renderLoggedOutMenu() {
        return <ul className='nav navbar-nav'>
            <li>
                <NavLink to={'/Console/Apps'} exact activeClassName='active'>
                    <span className='glyphicon glyphicon-home'></span> Apps
                </NavLink>
            </li>
            <li>
                <NavLink to={'/Console/Logout'} exact activeClassName='active'>
                    <span className='glyphicon glyphicon-log-out'></span> Logout
                </NavLink>
            </li>
        </ul>;
    }
}
