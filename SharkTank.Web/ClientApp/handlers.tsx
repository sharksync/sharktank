
class HttpError extends Error {
    response: Response;
}

export class ApiHandlers {

    static handleErrors(response: Response) {
        if (!response.ok) {
            throw Error(response.statusText);
        }
        return response;
    }
}