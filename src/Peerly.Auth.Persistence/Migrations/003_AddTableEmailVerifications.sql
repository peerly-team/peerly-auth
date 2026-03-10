-- +goose Up
-- +goose StatementBegin
create table email_verifications
(
    id                bigserial primary key not null,
    user_id           bigint                not null,
    token_hash        text                  not null,
    expiration_time   timestamptz           not null,
    verification_time timestamptz,
    creation_time     timestamptz           not null,
    update_time       timestamptz
);
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table email_verifications;
-- +goose StatementEnd
