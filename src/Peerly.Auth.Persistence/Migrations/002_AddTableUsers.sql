-- +goose Up
-- +goose StatementBegin
create table users
(
    id            bigserial primary key not null,
    email         text                  not null,
    password_hash text                  not null,
    name          text,
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table users;
-- +goose StatementEnd
