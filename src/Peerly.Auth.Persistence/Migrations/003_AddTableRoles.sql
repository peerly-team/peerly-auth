-- +goose Up
-- +goose StatementBegin
create table user_roles
(
    id            bigserial primary key not null,
    user_id       bigint                not null,
    role_id       int                   not null,
    creation_time timestamptz           not null
);

create table roles
(
    id          int primary key not null,
    name        text            not null,
    description text            not null
);

insert into roles (id, name, description) values (1, 'Admin', 'Администратор платформы');
insert into roles (id, name, description) values (2, 'Teacher', 'Преподаватель');
insert into roles (id, name, description) values (3, 'Student', 'Обучающийся');
-- +goose StatementEnd


-- +goose Down
-- +goose StatementBegin
drop table user_roles;
drop table roles;
-- +goose StatementEnd
