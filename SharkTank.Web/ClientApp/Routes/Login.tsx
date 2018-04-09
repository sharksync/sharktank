import * as React from 'react';
import * as FontAwesome from 'react-fontawesome';
import { RouteComponentProps } from 'react-router';
import { ApiHandlers } from '../handlers';

interface LoginState {
}

export class Login extends React.Component<RouteComponentProps<{}>, LoginState> {

    constructor() {
        super();

        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(provider: String, e: React.MouseEvent<HTMLButtonElement>) {
        window.location.href = ApiHandlers.Url + "Api/Auth/Start?provider=" + provider + "&returnUrl=" + window.location.href;
    }

    public render() {
        return <div>
            <h1>Login</h1>

            <p>Please select a provider to login:</p>

            <div id="openid-buttons">
                <button className="github-login" onClick={e => this.handleClick('GitHub', e)}>
                    <svg aria-hidden="true" className="svg-icon native iconGitHub" width="18" height="18" viewBox="0 0 16 16"><path fill-rule="evenodd" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0 0 16 8c0-4.42-3.58-8-8-8z"></path></svg>
                    &nbsp;GitHub
                </button>
                <button className="google-login" onClick={e => this.handleClick('Google', e)}>
                    <svg aria-hidden="true" className="svg-icon native iconGoogle" width="18" height="18" viewBox="0 0 18 18"><g><path d="M16.51 8H8.98v3h4.3c-.18 1-.74 1.48-1.6 2.04v2.01h2.6a7.8 7.8 0 0 0 2.38-5.88c0-.57-.05-.66-.15-1.18z" fill="#4285F4"></path><path d="M8.98 17c2.16 0 3.97-.72 5.3-1.94l-2.6-2a4.8 4.8 0 0 1-7.18-2.54H1.83v2.07A8 8 0 0 0 8.98 17z" fill="#34A853"></path><path d="M4.5 10.52a4.8 4.8 0 0 1 0-3.04V5.41H1.83a8 8 0 0 0 0 7.18l2.67-2.07z" fill="#FBBC05"></path><path d="M8.98 4.18c1.17 0 2.23.4 3.06 1.2l2.3-2.3A8 8 0 0 0 1.83 5.4L4.5 7.49a4.77 4.77 0 0 1 4.48-3.3z" fill="#EA4335"></path></g></svg>
                    &nbsp;Google
                </button>
                <button className="facebook-login" onClick={e => this.handleClick('Facebook', e)}>
                    <svg aria-hidden="true" className="svg-icon iconFacebook" width="18" height="18" viewBox="0 0 18 18"><path d="M1.88 1C1.4 1 1 1.4 1 1.88v14.24c0 .48.4.88.88.88h7.67v-6.2H7.46V8.4h2.09V6.61c0-2.07 1.26-3.2 3.1-3.2.88 0 1.64.07 1.87.1v2.16h-1.29c-1 0-1.19.48-1.19 1.18V8.4h2.39l-.31 2.42h-2.08V17h4.08c.48 0 .88-.4.88-.88V1.88c0-.48-.4-.88-.88-.88H1.88z" fill="#3C5A96"></path></svg>
                        &nbsp;Facebook
                </button>
            </div>

        </div>;
    }
}
