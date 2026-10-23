# Rule Traceability

Every business rule from [01-business-rules.md](01-business-rules.md) and the test files that reference it
(by rule ID in a comment or test name). CI runs `python3 build/check-rule-traceability.py` and fails when a
rule has no test. Regenerate this table with `python3 build/check-rule-traceability.py --markdown`.

| Rule | Tests |
|---|---|
| BR-C01 | CategoryTests, CreateCategoryCommandHandlerTests, CreateCategoryTests, RepositoryTests, UpdateCategoryCommandHandlerTests, UpdateCategoryTests |
| BR-C02 | CategoryTests, RepositoryTests, UpdateCategoryCommandHandlerTests, UpdateCategoryTests |
| BR-C03 | CategoryTests, CreateCategoryCommandHandlerTests, CreateCategoryCommandValidatorTests, CreateCategoryTests, UpdateCategoryCommandValidatorTests, UpdateCategoryTests |
| BR-C04 | DeleteCategoryCommandHandlerTests, DeleteCategoryTests, EndToEndTests, RepositoryTests |
| BR-C05 | DeleteCategoryCommandHandlerTests, DeleteCategoryTests, EndToEndTests |
| BR-C06 | ListCategoriesQueryHandlerTests, ListCategoriesTests |
| BR-C07 | GetCategoryQueryHandlerTests, GetCategoryTests, ListCategoriesTests, ProjectionTests, ReadDbContextTests |
| BR-G01 | CategoryTests, CreateCategoryCommandValidatorTests, CreateCategoryTests, CreateTaskCommandValidatorTests, CreateTaskTests, TaskItemCreateTests, UpdateCategoryCommandValidatorTests, UpdateCategoryTests, UpdateTaskTests |
| BR-G02 | EndToEndTests |
| BR-G03 | EndToEndTests, TaskItemCreateTests |
| BR-G04 | EndToEndTests |
| BR-L01 | ListCategoriesQueryValidatorTests, ListCategoriesTests, ListCategoryTasksTests, ListTasksQueryHandlerTests, ListTasksQueryValidatorTests, ListTasksTests, PagingTests |
| BR-L02 | ListCategoriesQueryHandlerTests, ListCategoriesTests, ListCategoryTasksTests, ListTasksQueryHandlerTests, ListTasksTests, PagingTests, ProjectionTests |
| BR-L03 | ListCategoryTasksTests, ListTasksQueryHandlerTests, ListTasksTests |
| BR-L04 | ListCategoryTasksTests, ListTasksQueryValidatorTests, ListTasksTests |
| BR-L05 | ListCategoryTasksTests, ListTasksQueryHandlerTests, ListTasksQueryValidatorTests, ListTasksTests |
| BR-L06 | ListCategoriesQueryHandlerTests, ListCategoriesTests |
| BR-T01 | CreateTaskCommandHandlerTests, CreateTaskTests, UpdateTaskCommandHandlerTests, UpdateTaskTests |
| BR-T02 | CreateTaskCommandHandlerTests, CreateTaskTests, EndToEndTests, TaskItemCreateTests |
| BR-T03 | CreateTaskCommandHandlerTests, CreateTaskTests, TaskItemCreateTests |
| BR-T04 | CreateTaskCommandHandlerTests, CreateTaskTests, TaskItemCreateTests, TaskItemEditTests, UpdateTaskCommandHandlerTests, UpdateTaskTests |
| BR-T05 | ChangeStatusCommandHandlerTests, ChangeTaskStatusTests, TaskItemStatusTests |
| BR-T06 | UpdateTaskTests |
| BR-T07 | ChangeStatusCommandHandlerTests, ChangeTaskStatusTests, TaskItemStatusTests |
| BR-T08 | TaskItemEditTests, UpdateTaskCommandHandlerTests, UpdateTaskTests |
| BR-T09 | DeleteTaskCommandHandlerTests, DeleteTaskTests, EndToEndTests |
| BR-T10 | TaskItemEditTests, UpdateTaskCommandHandlerTests, UpdateTaskTests |
