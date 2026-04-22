-- +goose Up
-- +goose StatementBegin
create table email_verifications
(
    user_id         bigint primary key not null,
    token           text               not null,
    expiration_time timestamptz        not null,
    process_status  text               not null,
    taken_time      timestamptz,
    fail_count      integer            not null,
    error           text,
    creation_time   timestamptz        not null,
    update_time     timestamptz
);
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table email_verifications;
-- +goose StatementEnd
