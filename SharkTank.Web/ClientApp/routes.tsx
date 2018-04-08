import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Apps } from './Routes/Apps';
import { Logout } from './Routes/Logout';

export const routes = <Layout>
    <Route path='/console/apps' component={Apps} />
    <Route path='/console/logout' component={Logout} />
</Layout>;
