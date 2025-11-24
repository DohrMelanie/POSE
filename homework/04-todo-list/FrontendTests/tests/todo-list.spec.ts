import { test, expect } from '@playwright/test';

test.describe('Todo List Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/todos');
  });

  test('should display the page title', async ({ page }) => {
    await expect(page.locator('h2')).toHaveText('Todo List');
  });

  test('should display empty state when no todos exist', async ({ page }) => {
    // Assuming the list starts empty or we clear it first
    const emptyMessage = page.locator('text=No todos yet. Add one above!');
    await expect(emptyMessage).toBeVisible();
  });

  test('should add a new todo successfully', async ({ page }) => {
    const titleInput = page.locator('input[placeholder="Title"]');
    const assigneeInput = page.locator('input[placeholder="Assignee"]');
    const addButton = page.locator('button.btn-add');

    // Fill in the form
    await titleInput.fill('Test Todo');
    await assigneeInput.fill('John Doe');

    // Click add button
    await addButton.click();

    // Verify the todo appears in the table
    await expect(page.locator('.todos-table')).toBeVisible();
    await expect(page.locator('td >> text=Test Todo')).toBeVisible();
    await expect(page.locator('td >> text=John Doe')).toBeVisible();
  });

  test('should clear input fields after adding a todo', async ({ page }) => {
    const titleInput = page.locator('input[placeholder="Title"]');
    const assigneeInput = page.locator('input[placeholder="Assignee"]');
    const addButton = page.locator('button.btn-add');

    await titleInput.fill('Test Todo');
    await assigneeInput.fill('John Doe');
    await addButton.click();

    // Wait for the todo to be added
    await expect(page.locator('td >> text=Test Todo')).toBeVisible();

    // Check if inputs are cleared
    await expect(titleInput).toHaveValue('');
    await expect(assigneeInput).toHaveValue('');
  });

  test('should toggle todo completion status', async ({ page }) => {
    // First add a todo
    await page.locator('input[placeholder="Title"]').fill('Complete Me');
    await page.locator('input[placeholder="Assignee"]').fill('Jane Doe');
    await page.locator('button.btn-add').click();

    await expect(page.locator('td >> text=Complete Me')).toBeVisible();

    // Find and click the checkbox
    const checkbox = page.locator('input[type="checkbox"]').first();
    await expect(checkbox).not.toBeChecked();
    
    await checkbox.check();
    await expect(checkbox).toBeChecked();

    // Uncheck it
    await checkbox.uncheck();
    await expect(checkbox).not.toBeChecked();
  });

  test('should update todo title inline', async ({ page }) => {
    // Add a todo first
    await page.locator('input[placeholder="Title"]').fill('Original Title');
    await page.locator('input[placeholder="Assignee"]').fill('Test User');
    await page.locator('button.btn-add').click();

    await expect(page.locator('td >> text=Original Title')).toBeVisible();

    // Find and edit the title
    const titleEditInput = page.locator('.edit-input').first();
    await titleEditInput.clear();
    await titleEditInput.fill('Updated Title');
    await titleEditInput.blur();

    // Verify the update
    await expect(page.locator('td >> text=Updated Title')).toBeVisible();
  });

  test('should update todo assignee inline', async ({ page }) => {
    // Add a todo first
    await page.locator('input[placeholder="Title"]').fill('Test Task');
    await page.locator('input[placeholder="Assignee"]').fill('Original Assignee');
    await page.locator('button.btn-add').click();

    await expect(page.locator('td >> text=Original Assignee')).toBeVisible();

    // Find and edit the assignee (second edit-input in the row)
    const assigneeEditInput = page.locator('.edit-input').nth(1);
    await assigneeEditInput.clear();
    await assigneeEditInput.fill('New Assignee');
    await assigneeEditInput.blur();

    // Verify the update
    await expect(page.locator('td >> text=New Assignee')).toBeVisible();
  });

  test('should delete a todo', async ({ page }) => {
    // Add a todo first
    await page.locator('input[placeholder="Title"]').fill('Delete Me');
    await page.locator('input[placeholder="Assignee"]').fill('Test User');
    await page.locator('button.btn-add').click();

    await expect(page.locator('td >> text=Delete Me')).toBeVisible();

    // Click delete button
    const deleteButton = page.locator('button.btn-delete').first();
    await deleteButton.click();

    // Verify the todo is removed
    await expect(page.locator('td >> text=Delete Me')).not.toBeVisible();
  });

  test('should display multiple todos', async ({ page }) => {
    // Add multiple todos
    for (let i = 1; i <= 3; i++) {
      await page.locator('input[placeholder="Title"]').fill(`Todo ${i}`);
      await page.locator('input[placeholder="Assignee"]').fill(`User ${i}`);
      await page.locator('button.btn-add').click();
      await page.waitForTimeout(100); // Small delay to ensure todos are added
    }

    // Verify all todos are visible
    const rows = page.locator('.todos-table tbody tr');
    await expect(rows).toHaveCount(3);
  });

  test('should maintain todo state after page reload', async ({ page }) => {
    // Add a todo
    await page.locator('input[placeholder="Title"]').fill('Persistent Todo');
    await page.locator('input[placeholder="Assignee"]').fill('Test User');
    await page.locator('button.btn-add').click();

    await expect(page.locator('td >> text=Persistent Todo')).toBeVisible();

    // Reload the page
    await page.reload();

    // Verify the todo still exists
    await expect(page.locator('td >> text=Persistent Todo')).toBeVisible();
  });
});
