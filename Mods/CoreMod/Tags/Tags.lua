coremod.desert =
{
	criteria = 
	{
		min_temperature = 30,
		max_temperature = 50,
		min_humidity = 0,
		max_humidity = 0.5
	},
	expression = expressions.temp_hum__expression
}

coremod.mountains =
{
	criteria = 
	{
		min_height = 800,
		max_height = 4000
	},
	expression = expressions.height_expression
}

coremod.hills =
{
	criteria = 
	{
		min_height = 300,
		max_height = 800
	},
	expression = expressions.height_expression
}

coremod.plains =
{
	criteria = 
	{
		min_temperature = 0,
		max_temperature = 30,
		min_height = 0,
		max_height = 300
	},
	expression = expressions.plains_expression
}

coremod.irradiated =
{
	criteria = 
	{
		min_radioactivity = 0.2,
		max_radioactivity = 0.5
	},
	expression = expressions.rad_expression
}

coremod.swamp =
{
	criteria = 
	{
		min_temperature = 20,
		max_temperature = 35,
		min_height = 0,
		max_height = 300,
		min_humidity = 0.5,
		max_humidity = 1
	},
	expression = expressions.swamp_expression
}

coremod.steppe =
{
	criteria = 
	{
		min_temperature = 0,
		max_temperature = 40,
		min_height = 0,
		max_height = 300,
		min_humidity = 0,
		max_humidity = 0.5,
		min_inlandness = 0.5,
		max_inlandness = 1
	},
	expression = expressions.steppe_expression
}

coremod.grassland =
{
	criteria = 
	{
		min_temperature = 18,
		max_temperature = 30,
		min_humidity = 0.5,
		max_humidity = 1
	},
	expression = expressions.temp_hum__expression
}


coremod.land =
{
	criteria = 
	{
		slot_surface = defines.LAND_SURFACE
	},
	expression = expressions.surface_expression
}

coremod.ocean =
{
	criteria =
	{
		slot_surface = defines.OCEAN_SURFACE
	},
	expression = expressions.surface_expression
}

coremod.region =
{
	criteria = 
	{

	},
	expression = function (slot, criteria)
		tiles = slot.Get(component.Region)
		if tiles == nil then
			return false
		else
			return tiles.is_region
		end
	end
}

coremod.singular =
{
	criteria = 
	{

	},
	expression = function (slot, criteria)
		tiles = slot.Get(component.Region)
		if tiles == nil then
			return false
		else
			return tiles.size < 1.1 and tiles.size > 0.9 and not tiles.is_region
		end
	end
}

coremod.multiple =
{
	criteria = 
	{

	},
	expression = function (slot, criteria)
		tiles = slot.Get(component.Region)
		if tiles == nil then
			return false
		else
			return tiles.size > 1 and not tiles.is_region
		end
	end
}


coremod.nest = 
{
	criteria = {},
	expression = function ( slot, criteria )
		nestCmp = slot.Get(component.Nest)
		if nestCmp == nil then
			return false
		else
			return true
		end
	end
}