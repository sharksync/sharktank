import * as React from 'react';
import * as FontAwesome from 'react-fontawesome';
require('font-awesome/css/font-awesome.css');

interface DeleteButtonState {
    loading: boolean;
    deleteHandler: DeleteCallback;
}

interface DeleteButtonProps {
    deleteHandler: DeleteCallback;
}

type DeleteCallback = (completedCallback: () => any) => any;

export class DeleteButton extends React.Component<DeleteButtonProps, DeleteButtonState> {
    constructor(props: DeleteButtonProps) {
        super();

        this.state = { loading: false, deleteHandler: props.deleteHandler };
        this.handleClick = this.handleClick.bind(this);
        this.disableLoading = this.disableLoading.bind(this);
    }

    handleClick() {
        if (!this.state.loading) {
            this.setState({ loading: true });

            if (this.state.deleteHandler)
                this.state.deleteHandler(this.disableLoading);
        }
    }

    disableLoading() {
        this.setState({ loading: false });
    }

    render() {
        let isLoading = this.state.loading;
        return <button className="btn btn-danger" disabled={isLoading} onClick={this.handleClick}>
            {isLoading ? <span><FontAwesome name="spinner" spin /> Delete</span> : 'Delete'}
        </button>;
    }
}
