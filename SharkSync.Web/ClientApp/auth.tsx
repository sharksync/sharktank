import { ApiHandlers } from './handlers';

type userDetailsCallback = (loggedInUser: LoggedInUser | null) => any;

interface AuthDetailsResponse {
    LoggedInUser: LoggedInUser;
    Success: boolean;
}

export interface LoggedInUser {
    Id: string;
    Name: string;
    EmailAddress: string;
    AccountType: string;
    HasAvatarUrl: boolean;
    XSRFToken: string;
}

export class Auth {

    public static getLoggedInUserFromCache(): LoggedInUser | null {

        var stringOfUser = localStorage.getItem('loggedInUser');
        var user = null;

        if (stringOfUser)
            user = JSON.parse(stringOfUser);

        return user;
    }
    
    public static putLoggedInUserIntoCache(callback: userDetailsCallback) {
        
        fetch(ApiHandlers.Url + 'Api/Auth/Details', {
            method: 'GET',
            headers: ApiHandlers.GetStandardHeaders(),
            credentials: 'include'
        })
            .then(response => response.json() as Promise<AuthDetailsResponse>)
            .then(data => {

                localStorage.setItem('loggedInUser', JSON.stringify(data.LoggedInUser));

                callback(data.LoggedInUser);
            }).catch(ex => {
                callback(null);
            });

    }

    public static clearLoggedInCache() {

        localStorage.removeItem('loggedInUser');

    }

}