import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Apps } from './Routes/Apps';
import { Login } from './Routes/Login';
import { LoginComplete } from './Routes/LoginComplete';
import { Logout } from './Routes/Logout';

export const routes = <Layout>
    <Route path='/Console/Apps' component={Apps} />
    <Route path='/Console/LoginComplete' component={LoginComplete} />
    <Route path='/Console/Login' component={Login} />
    <Route path='/Console/Logout' component={Logout} />
</Layout>;