export interface User {
    username: string;
    token: string;
    photoUrl: string;
    knownAs: string;
    gender: string;
    roles: string[];

}
export interface PasswordChange {
    username: string;
    password: string;
}