
import swal from 'sweetalert2';

class HttpError extends Error {
    response: Response;
}

export class ApiHandlers {

    static Url = "https://js3pdj4u57.execute-api.eu-west-1.amazonaws.com/Prod/";

    static handleErrors(response: Response) {
        if (!response.ok) {
            swal(
                'Failed action',
                'Failed to complete action, please try again.',
                'error'
            )
            throw Error(response.statusText);
        }
        return response;
    }

    static handleCatch(error: Error) {
        if (error) {
            swal(
                'Failed action',
                'Failed to complete action, please try again.',
                'error'
            )
            throw error;
        }
    }
}