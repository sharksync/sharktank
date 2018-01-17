import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Apps } from './components/Apps';

export const routes = <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/fetchdata' component={FetchData} />
    <Route path='/apps' component={Apps} />
</Layout>;
