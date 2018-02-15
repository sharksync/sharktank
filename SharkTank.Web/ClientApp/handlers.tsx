
import swal from 'sweetalert2';

class HttpError extends Error {
    response: Response;
}

export class ApiHandlers {

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
}