import * as React from 'react';
import { NavMenu } from './NavMenu';
import { RouteProps } from 'react-router-dom';

interface LayoutProps {
    children?: React.ReactNode;
}

export class Layout extends React.Component<{}, {}> {

    public render() {
        return <div className='container-fluid'>
            <div className='row'>
                <div className='col-sm-3'>
                    <NavMenu />
                </div>
                <div className='col-sm-9'>
                    { this.props.children }
                </div>
            </div>
        </div>;
    }
}
