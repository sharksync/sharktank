import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Apps } from './components/Apps';

export const routes = <Layout>
    <Route exact path='/' component={Apps} />
</Layout>;
