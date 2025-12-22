-- ============================================
-- Add IsDeleted column to Reviews table
-- This column supports soft delete functionality
-- ============================================

USE SmartShopDB;

-- Add IsDeleted column to Reviews table
ALTER TABLE Reviews 
ADD COLUMN IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
AFTER HelpfulCount;

-- Create index for better performance on filtering deleted reviews
CREATE INDEX idx_reviews_isdeleted ON Reviews(IsDeleted);

-- Update existing reviews to set IsDeleted = FALSE (if any exist)
UPDATE Reviews SET IsDeleted = FALSE WHERE IsDeleted IS NULL;

SELECT 'IsDeleted column added to Reviews table successfully!' AS Status;

-- Verify the change
DESCRIBE Reviews;
