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
    AvatarUrl: string;
    XSRFToken: string;
}

export class Auth {

    public static getLoggedInUserFromCache(): LoggedInUser | null {

        var id = localStorage.getItem('loggedInUserId');
        var name = localStorage.getItem('loggedInUserName');
        var email = localStorage.getItem('loggedInUserEmail');
        var avatarUrl = localStorage.getItem('loggedInUserAvatarUrl');
        var xsrfToken = localStorage.getItem('loggedInUserXSRFToken');

        if (id && name && email && avatarUrl && xsrfToken) {

            return {
                Id: id,
                Name: name,
                EmailAddress: email,
                AvatarUrl: avatarUrl,
                XSRFToken: xsrfToken
            };
        }

        return null;
    }
    
    public static putLoggedInUserIntoCache(callback: userDetailsCallback) {
        
        fetch(ApiHandlers.Url + 'Api/Auth/Details', {
            method: 'GET',
            headers: ApiHandlers.GetStandardHeaders(),
            credentials: 'include'
        })
            .then(response => response.json() as Promise<AuthDetailsResponse>)
            .then(data => {

                localStorage.setItem('loggedInUserId', data.LoggedInUser.Id);
                localStorage.setItem('loggedInUserName', data.LoggedInUser.Name);
                localStorage.setItem('loggedInUserEmail', data.LoggedInUser.EmailAddress);
                localStorage.setItem('loggedInUserAvatarUrl', data.LoggedInUser.AvatarUrl);
                localStorage.setItem('loggedInUserXSRFToken', data.LoggedInUser.XSRFToken);

                callback(data.LoggedInUser);
            }).catch(ex => {
                callback(null);
            });

    }

}