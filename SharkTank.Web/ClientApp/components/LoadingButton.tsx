import * as React from 'react';
import FontAwesome from 'react-fontawesome';

interface LoadingButtonState {
    loading: boolean;
}

export class LoadingButton extends React.Component<{}, LoadingButtonState> {
    constructor() {
        super();

        this.state = { loading: false };
    }

    handleClick() {
        if (!this.state.loading) {
            this.setState({ loading: true });

            // This probably where you would have an `ajax` call
            setTimeout(() => {
                // Completed of async action, set loading state back
                this.setState({ loading: false });
            }, 2000);
        }
    }

    render() {
        let isLoading = this.state.loading;
        return <button className="btn btn-danger" disabled={isLoading} onClick={this.handleClick.bind(this)}>
            {isLoading ? <FontAwesome name="rocket" /> : 'Button'}
        </button >;
    }
}
