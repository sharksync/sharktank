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

    static handleErrors(response: Response) {
        if (!response.ok) {

            // Need to sign in?
            if (response.status == 401) {

                // Clear stored auth details
                localStorage.removeItem('loggedInUserId');
                localStorage.removeItem('loggedInUserName');
                localStorage.removeItem('loggedInUserEmail');
                localStorage.removeItem('loggedInUserAvatarUrl');

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