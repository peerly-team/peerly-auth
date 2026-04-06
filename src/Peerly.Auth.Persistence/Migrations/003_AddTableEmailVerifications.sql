-- +goose Up
-- +goose StatementBegin
create table email_verifications
(
    id                bigserial   primary key not null,
    user_id           bigint                  not null,
    token             text                    not null,
    expiration_time   timestamptz             not null,
    verification_time timestamptz,
    process_status    text                    not null,
    taken_time        timestamptz,
    fail_count        integer                 not null,
    error             text,
    creation_time     timestamptz             not null,
    update_time       timestamptz
);

create index idx_email_verifications_unprocessed
    on email_verifications (id)
    where process_status in ('Created', 'Failed');
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table email_verifications;
-- +goose StatementEnd
