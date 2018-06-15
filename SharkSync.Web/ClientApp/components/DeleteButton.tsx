import * as React from 'react';
import * as FontAwesome from 'react-fontawesome';
import swal from 'sweetalert2';
import 'font-awesome/css/font-awesome.css';

interface DeleteButtonState {
    loading: boolean;
    deleteHandler: DeleteCallback;
    confirmMessage: string;
}

interface DeleteButtonProps {
    deleteHandler: DeleteCallback;
    confirmMessage: string;
}

type DeleteCallback = (completedCallback: () => any) => any;

export class DeleteButton extends React.Component<DeleteButtonProps, DeleteButtonState> {
    constructor(props: DeleteButtonProps) {
        super();

        this.state = { loading: false, deleteHandler: props.deleteHandler, confirmMessage: props.confirmMessage };
        this.handleClick = this.handleClick.bind(this);
        this.disableLoading = this.disableLoading.bind(this);
    }

    handleClick() {
        if (!this.state.loading) {
            swal({
                title: 'Confirm delete',
                text: this.state.confirmMessage,
                type: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, delete it!',
                confirmButtonColor: '#d9534f'
            }).then((result) => {
                if (result.value) {

                    this.setState({ loading: true });

                    if (this.state.deleteHandler)
                        this.state.deleteHandler(this.disableLoading);
                }
            })
        }
    }

    disableLoading() {
        this.setState({ loading: false });
    }

    render() {
        let isLoading = this.state.loading;

        return <div>
            <button className="btn btn-danger" disabled={isLoading} onClick={this.handleClick}>
                {isLoading ? <span><FontAwesome name="spinner" spin /> Deleting...</span> : 'Delete'}
            </button>
        </div>;
    }
}
