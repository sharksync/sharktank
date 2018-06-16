import { Auth } from './auth';
import swal from 'sweetalert2';

interface HttpError extends Error {
    response: Response;
}

interface UnauthorizedResponse {
    ChallengeUrl: string;
}

declare var WEB_API_URL: string;

export class ApiHandlers {

    static Url = WEB_API_URL;

    static GetStandardHeaders() {

        var xsrfToken = localStorage.getItem('loggedInUserXSRFToken') || '';

        return {
            'Accept': 'application/json',
            'Cache': 'no-cache',
            'X-XSRF-TOKEN': xsrfToken
        };
    }

    static handleErrors(response: Response) {
        if (!response.ok) {

            // Need to sign in?
            if (response.status == 401) {

                Auth.clearLoggedInCache();
                
                // Auth required, redirect to location header
                window.location.href = "/Console/Login";
                throw Error("Unauthorized");

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
        if (error && error.message != "Unauthorized") {
            swal(
                'Failed action',
                'Failed to complete action, please try again.',
                'error'
            )
            throw error;
        }
    }
}