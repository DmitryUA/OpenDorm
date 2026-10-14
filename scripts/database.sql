-- =================================================================================================
-- BASELINE SCHEMA SNAPSHOT
-- Generated: 2026-10-14
-- WARNING: This is a point-in-time snapshot. This snapshot does not contain the EFMigrations table.
-- If you need to apply future changes, please use EF Core migrations 
-- or ask the team to generate an updated snapshot.
-- =================================================================================================

-- DROP SCHEMA public;

CREATE SCHEMA public AUTHORIZATION pg_database_owner;

COMMENT ON SCHEMA public IS 'standard public schema';

-- public.dormitories определение

-- Drop table

-- DROP TABLE public.dormitories;

CREATE TABLE public.dormitories (
	id uuid NOT NULL, -- идентификатор общежития
	address_city varchar(50) NOT NULL, -- город
	address_street varchar(255) NOT NULL, -- улица
	address_house varchar(20) NOT NULL, -- номер дома
	floor_count int4 NOT NULL, -- общее количество этажей в здании
	CONSTRAINT "CK_Address_City_MinLength" null,
	CONSTRAINT "CK_Address_Street_MinLength" null,
	CONSTRAINT "PK_dormitories" null
);
COMMENT ON TABLE public.dormitories IS 'общежития';

-- Column comments

COMMENT ON COLUMN public.dormitories.id IS 'идентификатор общежития';
COMMENT ON COLUMN public.dormitories.address_city IS 'город';
COMMENT ON COLUMN public.dormitories.address_street IS 'улица';
COMMENT ON COLUMN public.dormitories.address_house IS 'номер дома';
COMMENT ON COLUMN public.dormitories.floor_count IS 'общее количество этажей в здании';


-- public.occupants определение

-- Drop table

-- DROP TABLE public.occupants;

CREATE TABLE public.occupants (
	id uuid NOT NULL, -- идентификатор жильца
	last_name_encrypted bytea NOT NULL, -- зашифрованная фамилия
	first_name_encrypted bytea NOT NULL, -- зашифрованное имя
	patronymic_encrypted bytea NULL, -- зашифрованное отчество
	gender bpchar(1) NOT NULL, -- гендер (m - мужчина, f - женщина)
	birth_date_encrypted bytea NOT NULL, -- зашифрованная дата рождения
	is_active bool NOT NULL, -- активен ли проживающий, true - да, false - нет
	CONSTRAINT "PK_occupants" null
);
COMMENT ON TABLE public.occupants IS 'жильцы';

-- Column comments

COMMENT ON COLUMN public.occupants.id IS 'идентификатор жильца';
COMMENT ON COLUMN public.occupants.last_name_encrypted IS 'зашифрованная фамилия';
COMMENT ON COLUMN public.occupants.first_name_encrypted IS 'зашифрованное имя';
COMMENT ON COLUMN public.occupants.patronymic_encrypted IS 'зашифрованное отчество';
COMMENT ON COLUMN public.occupants.gender IS 'гендер (m - мужчина, f - женщина)';
COMMENT ON COLUMN public.occupants.birth_date_encrypted IS 'зашифрованная дата рождения';
COMMENT ON COLUMN public.occupants.is_active IS 'активен ли проживающий, true - да, false - нет';


-- public.roles определение

-- Drop table

-- DROP TABLE public.roles;

CREATE TABLE public.roles (
	id uuid NOT NULL, -- идентификатор роли
	"name" int4 NOT NULL,
	normalized_name varchar(64) NOT NULL,
	CONSTRAINT "PK_roles" PRIMARY KEY (id)
);
CREATE UNIQUE INDEX "IX_roles_name" ON public.roles USING btree (name);
COMMENT ON TABLE public.roles IS 'роли';

-- Column comments

COMMENT ON COLUMN public.roles.id IS 'идентификатор роли';


-- public.system_flags определение

-- Drop table

-- DROP TABLE public.system_flags;

CREATE TABLE public.system_flags (
	id uuid NOT NULL, -- идентификатор флага
	"name" varchar(128) NOT NULL,
	value varchar(512) NOT NULL,
	created_at timestamptz NOT NULL,
	CONSTRAINT "PK_system_flags" null
);
CREATE UNIQUE INDEX "IX_system_flags_name" ON public.system_flags ("name" text_ops);

-- Column comments

COMMENT ON COLUMN public.system_flags.id IS 'идентификатор флага';


-- public.users определение

-- Drop table

-- DROP TABLE public.users;

CREATE TABLE public.users (
	id uuid NOT NULL, -- идентификатор пользователя
	last_name_encrypted bytea NOT NULL, -- зашифрованная фамилия
	first_name_encrypted bytea NOT NULL, -- зашифрованное имя
	patronymic_encrypted bytea NULL, -- зашифрованное отчество
	is_active bool NOT NULL,
	created_at timestamptz NOT NULL,
	login varchar(20) NOT NULL,
	password_hash varchar(255) NOT NULL,
	CONSTRAINT "CK_Login_MinLength" null,
	CONSTRAINT "PK_users" null
);
COMMENT ON TABLE public.users IS 'пользователи';

-- Column comments

COMMENT ON COLUMN public.users.id IS 'идентификатор пользователя';
COMMENT ON COLUMN public.users.last_name_encrypted IS 'зашифрованная фамилия';
COMMENT ON COLUMN public.users.first_name_encrypted IS 'зашифрованное имя';
COMMENT ON COLUMN public.users.patronymic_encrypted IS 'зашифрованное отчество';


-- public.refresh_tokens определение

-- Drop table

-- DROP TABLE public.refresh_tokens;

CREATE TABLE public.refresh_tokens (
	id uuid NOT NULL, -- идентификатор токена
	user_id uuid NOT NULL,
	"token" varchar(255) NOT NULL,
	expires_at timestamptz NOT NULL,
	is_revoked bool NOT NULL,
	created_at timestamptz NOT NULL,
	CONSTRAINT "PK_refresh_tokens" null,
	CONSTRAINT "FK_refresh_tokens_users" FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_refresh_tokens_token" ON public.refresh_tokens ("token" text_ops);
CREATE INDEX "IX_refresh_tokens_user_id" ON public.refresh_tokens (user_id uuid_ops);
COMMENT ON TABLE public.refresh_tokens IS 'токены обновления';

-- Column comments

COMMENT ON COLUMN public.refresh_tokens.id IS 'идентификатор токена';


-- public.rooms определение

-- Drop table

-- DROP TABLE public.rooms;

CREATE TABLE public.rooms (
	id uuid NOT NULL, -- идентификатор комнаты
	capacity int4 NOT NULL, -- вместимость
	gender bpchar(1) NOT NULL, -- гендер, m - мужская, f - женская
	floor_number int4 NOT NULL, -- номер этажа
	is_active bool DEFAULT true NOT NULL, -- активна ли комната
	dormitory_id uuid NOT NULL, -- идентификатор общежития
	"name" varchar(128) NOT NULL, -- номер комнаты
	CONSTRAINT "CK_Name_MinLength" null,
	CONSTRAINT "PK_rooms" null,
	CONSTRAINT "FK_rooms_dormitories_dormitory_id" FOREIGN KEY (dormitory_id) REFERENCES public.dormitories(id) ON DELETE CASCADE
);
CREATE INDEX "IX_rooms_dormitory_id" ON public.rooms (dormitory_id uuid_ops);
COMMENT ON TABLE public.rooms IS 'комнаты в общежитиях';

-- Column comments

COMMENT ON COLUMN public.rooms.id IS 'идентификатор комнаты';
COMMENT ON COLUMN public.rooms.capacity IS 'вместимость';
COMMENT ON COLUMN public.rooms.gender IS 'гендер, m - мужская, f - женская';
COMMENT ON COLUMN public.rooms.floor_number IS 'номер этажа';
COMMENT ON COLUMN public.rooms.is_active IS 'активна ли комната';
COMMENT ON COLUMN public.rooms.dormitory_id IS 'идентификатор общежития';
COMMENT ON COLUMN public.rooms."name" IS 'номер комнаты';


-- public.user_roles определение

-- Drop table

-- DROP TABLE public.user_roles;

CREATE TABLE public.user_roles (
	id uuid NOT NULL, -- идентификатор роли пользователя
	user_id uuid NOT NULL,
	role_id uuid NOT NULL,
	CONSTRAINT "PK_user_roles" null,
	CONSTRAINT "FK_user_roles_roles" FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE RESTRICT,
	CONSTRAINT "FK_user_roles_users" FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);
CREATE INDEX "IX_user_roles_role_id" ON public.user_roles (role_id uuid_ops);
CREATE UNIQUE INDEX "IX_user_roles_user_id_role_id" ON public.user_roles (user_id uuid_ops,role_id uuid_ops);
COMMENT ON TABLE public.user_roles IS 'роли пользователя';

-- Column comments

COMMENT ON COLUMN public.user_roles.id IS 'идентификатор роли пользователя';


-- public.accommodations определение

-- Drop table

-- DROP TABLE public.accommodations;

CREATE TABLE public.accommodations (
	id uuid NOT NULL, -- идентификатор заселения
	room_id uuid NOT NULL, -- идентификатор комнаты
	check_in_date timestamptz NOT NULL, -- дата и время заселения
	check_out_date timestamptz NULL, -- дата и время выселения
	occupant_id uuid NOT NULL, -- идентификатор жильца
	CONSTRAINT "PK_accommodations" null,
	CONSTRAINT "FK_accommodations_occupants_occupant_id" FOREIGN KEY (occupant_id) REFERENCES public.occupants(id) ON DELETE CASCADE,
	CONSTRAINT "FK_accommodations_rooms_room_id" FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON DELETE RESTRICT
);
CREATE INDEX "IX_accommodations_room_id" ON public.accommodations (room_id uuid_ops);
CREATE INDEX "UX_Accommodations_OccupantId_Active" ON public.accommodations (occupant_id uuid_ops);
COMMENT ON TABLE public.accommodations IS 'заселения';

-- Column comments

COMMENT ON COLUMN public.accommodations.id IS 'идентификатор заселения';
COMMENT ON COLUMN public.accommodations.room_id IS 'идентификатор комнаты';
COMMENT ON COLUMN public.accommodations.check_in_date IS 'дата и время заселения';
COMMENT ON COLUMN public.accommodations.check_out_date IS 'дата и время выселения';
COMMENT ON COLUMN public.accommodations.occupant_id IS 'идентификатор жильца';
