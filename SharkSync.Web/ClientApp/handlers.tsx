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