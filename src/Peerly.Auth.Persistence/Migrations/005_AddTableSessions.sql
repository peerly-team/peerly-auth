-- +goose Up
-- +goose StatementBegin
create table sessions
(
    id                 bigserial primary key not null,
    user_id            bigint                not null,
    refresh_token_hash text                  not null,
    expiration_time    timestamptz           not null,
    creation_time      timestamptz           not null
);
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table sessions;
-- +goose StatementEnd
