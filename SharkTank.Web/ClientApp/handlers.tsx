
import swal from 'sweetalert2';

class HttpError extends Error {
    response: Response;
}

interface UnauthorizedResponse {
    ChallengeUrl: string;
}

export class ApiHandlers {

    //static Url = "https://z923hkq2sg.execute-api.eu-west-1.amazonaws.com/Prod/";
    static Url = "http://localhost:57829/";

    static handleErrors(response: Response) {
        if (!response.ok) {

            if (response.status == 401) {
                // Auth required, redirect to location header
                (response.json() as Promise<UnauthorizedResponse>)
                    .then(unauthorised => window.location.href = unauthorised.ChallengeUrl);
            } else {
                swal(
                    'Failed action',
                    'Failed to complete action, please try again.',
                    'error'
                )
                throw Error(response.statusText);
            }
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