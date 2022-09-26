CREATE TABLE department
    (id int(11), name varchar(100))
;


CREATE TABLE employee
    (id int(11), department_id int(11), chief_id int(11), name varchar(100), salary int(11))
;

INSERT INTO department
    (id, name)
VALUES
    (1, "D1"),
    (2, "D2"),
    (3, "D3")
;

INSERT INTO employee
    (id, department_id, chief_id, name, salary)
VALUES
    (1, 1, 5, "John", 100),
    (2, 1, 5, "Misha", 600),
    (3, 2, 6, "Eugen", 300),
    (4, 2, 6, "Tolya", 400),
    (5, 3, 7, "Stepan", 500),
	(6, 3, 7, "Alex", 1000),
	(7, 3, NULL, "Ivan", 1100);


